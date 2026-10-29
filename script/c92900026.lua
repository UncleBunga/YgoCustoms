-- Slasher's Chest of Souls
local s,id=GetID()
function s.initial_effect(c)
    -- Activate: Special Summon 1 "Slasher" from GY
    local e1=Effect.CreateEffect(c)
    e1:SetCategory(CATEGORY_SPECIAL_SUMMON+CATEGORY_POSITION)
    e1:SetType(EFFECT_TYPE_ACTIVATE)
    e1:SetProperty(EFFECT_FLAG_CARD_TARGET)
    e1:SetCode(EVENT_FREE_CHAIN)
    e1:SetTarget(s.target)
    e1:SetOperation(s.activate)
    c:RegisterEffect(e1)
end

function s.filter(c,e,tp)
    return c:IsSetCard(0x991) and c:IsCanBeSpecialSummoned(e,0,tp,false,false)
end

function s.target(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
    if chkc then return chkc:IsLocation(LOCATION_GRAVE) and chkc:IsControler(tp) and s.filter(chkc,e,tp) end
    if chk==0 then return Duel.GetLocationCount(tp,LOCATION_MZONE)>0
        and Duel.IsExistingTarget(s.filter,tp,LOCATION_GRAVE,0,1,nil,e,tp) end
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_SPSUMMON)
    local g=Duel.SelectTarget(tp,s.filter,tp,LOCATION_GRAVE,0,1,1,nil,e,tp)
    Duel.SetOperationInfo(0,CATEGORY_SPECIAL_SUMMON,g,1,0,0)
end

function s.activate(e,tp,eg,ep,ev,re,r,rp)
    local c=e:GetHandler()
    local tc=Duel.GetFirstTarget()
    if tc:IsRelateToEffect(e) and Duel.SpecialSummon(tc,0,tp,tp,false,false,POS_FACEUP)>0 then
        
        -- Baseline: Change 1 opponent's monster to face-down Defense Position
        local pg=Duel.GetMatchingGroup(Card.IsCanTurnSet,tp,0,LOCATION_MZONE,nil)
        if #pg>0 and Duel.SelectYesNo(tp,aux.Stringid(id,0)) then -- "Change monster to face-down?" string
            Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_POSCHANGE)
            local sg=pg:Select(tp,1,1,nil)
            Duel.HintSelection(sg)
            Duel.ChangePosition(sg,POS_FACEDOWN_DEFENSE)
        end

        -- Bonus: If it's a Level 8 Fiend, grant scaling ATK and End Phase destruction
        if tc:IsLevel(8) and tc:IsRace(RACE_FIEND) then
            -- Gains 100 ATK for each monster in the GYs
            local e1=Effect.CreateEffect(c)
            e1:SetType(EFFECT_TYPE_SINGLE)
            e1:SetProperty(EFFECT_FLAG_SINGLE_RANGE)
            e1:SetRange(LOCATION_MZONE)
            e1:SetCode(EFFECT_UPDATE_ATTACK)
            e1:SetValue(s.atkval)
            e1:SetReset(RESET_EVENT+RESETS_STANDARD)
            tc:RegisterEffect(e1,true)

            -- End Phase Destroy effect
            local e2=Effect.CreateEffect(c)
            e2:SetDescription(aux.Stringid(id,1)) -- "Destroy 1 card" string
            e2:SetCategory(CATEGORY_DESTROY)
            e2:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_TRIGGER_O)
            e2:SetCode(EVENT_PHASE+PHASE_END)
            e2:SetRange(LOCATION_MZONE)
            e2:SetCountLimit(1)
            e2:SetTarget(s.destg)
            e2:SetOperation(s.desop)
            e2:SetReset(RESET_EVENT+RESETS_STANDARD)
            tc:RegisterEffect(e2,true)
            
            -- Inform the player the monster gained an effect
            tc:RegisterFlagEffect(0,RESET_EVENT+RESETS_STANDARD,EFFECT_FLAG_CLIENT_HINT,1,0,aux.Stringid(id,2)) -- "Gained End Phase Effect"
        end
    end
end

-- ATK calculation: 100 * total monsters in both GYs
function s.atkval(e,c)
    return Duel.GetMatchingGroupCount(Card.IsType,c:GetControler(),LOCATION_GRAVE,LOCATION_GRAVE,nil,TYPE_MONSTER)*100
end

-- End Phase Destruction target
function s.destg(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return Duel.IsExistingMatchingCard(Card.IsDestructable,tp,LOCATION_ONFIELD,LOCATION_ONFIELD,1,nil) end
    Duel.SetOperationInfo(0,CATEGORY_DESTROY,nil,1,0,LOCATION_ONFIELD)
end

-- End Phase Destruction operation
function s.desop(e,tp,eg,ep,ev,re,r,rp)
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_DESTROY)
    local g=Duel.SelectMatchingCard(tp,Card.IsDestructable,tp,LOCATION_ONFIELD,LOCATION_ONFIELD,1,1,nil)
    if #g>0 then
        Duel.HintSelection(g)
        Duel.Destroy(g,REASON_EFFECT)
    end
end