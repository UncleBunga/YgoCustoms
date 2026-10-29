-- Curse of Thorn
local s,id=GetID()
function s.initial_effect(c)
    -- Activate
    local e1=Effect.CreateEffect(c)
    e1:SetCategory(CATEGORY_SPECIAL_SUMMON+CATEGORY_COUNTER)
    e1:SetType(EFFECT_TYPE_ACTIVATE)
    e1:SetProperty(EFFECT_FLAG_CARD_TARGET)
    e1:SetCode(EVENT_FREE_CHAIN)
    e1:SetTarget(s.target)
    e1:SetOperation(s.activate)
    c:RegisterEffect(e1)
end

function s.spfilter(c,e,tp)
    return c:IsSetCard(0x991) and c:IsCanBeSpecialSummoned(e,0,tp,false,false)
end

function s.target(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
    if chkc then return chkc:IsLocation(LOCATION_GRAVE) and chkc:IsControler(tp) and s.spfilter(chkc,e,tp) end
    if chk==0 then return Duel.GetLocationCount(tp,LOCATION_MZONE)>0
        and Duel.IsExistingTarget(s.spfilter,tp,LOCATION_GRAVE,0,1,nil,e,tp) end
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_SPSUMMON)
    local g=Duel.SelectTarget(tp,s.spfilter,tp,LOCATION_GRAVE,0,1,1,nil,e,tp)
    Duel.SetOperationInfo(0,CATEGORY_SPECIAL_SUMMON,g,1,0,0)
end

function s.activate(e,tp,eg,ep,ev,re,r,rp)
    local c=e:GetHandler()
    local tc=Duel.GetFirstTarget()

    if tc and tc:IsRelateToEffect(e) and Duel.SpecialSummon(tc,0,tp,tp,false,false,POS_FACEUP)>0 then
        -- Place a Kill Counter on 1 face-up card on the field
        local cg=Duel.GetMatchingGroup(Card.IsCanAddCounter,tp,LOCATION_ONFIELD,LOCATION_ONFIELD,nil,0x199,1)
        if #cg>0 and Duel.SelectYesNo(tp,aux.Stringid(id,0)) then -- "Do you want to place a Kill Counter?"
            Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_COUNTER)
            local sg=cg:Select(tp,1,1,nil)
            if #sg>0 then
                Duel.HintSelection(sg)
                sg:GetFirst():AddCounter(0x199,1)
            end
        end

        -- Check if it's a Level 8 Warrior
        if tc:IsLevel(8) and tc:IsRace(RACE_WARRIOR) then
            -- Effect 1: Opponent cannot activate effects during the Battle Phase
            local e1=Effect.CreateEffect(c)
            e1:SetDescription(aux.Stringid(id,1))
            e1:SetType(EFFECT_TYPE_FIELD)
            e1:SetProperty(EFFECT_FLAG_PLAYER_TARGET+EFFECT_FLAG_CLIENT_HINT)
            e1:SetCode(EFFECT_CANNOT_ACTIVATE)
            e1:SetRange(LOCATION_MZONE)
            e1:SetTargetRange(0,1)
            e1:SetCondition(s.bpcon)
            e1:SetValue(1)
            e1:SetReset(RESET_EVENT+RESETS_STANDARD)
            tc:RegisterEffect(e1,true)

            -- Effect 2: Gain Kill Counter when one is removed
            local e2=Effect.CreateEffect(c)
            e2:SetDescription(aux.Stringid(id,2))
            e2:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_TRIGGER_O)
            e2:SetCode(EVENT_REMOVE_COUNTER+0x199)
            e2:SetProperty(EFFECT_FLAG_DELAY)
            e2:SetRange(LOCATION_MZONE)
            e2:SetCountLimit(1)
            e2:SetCondition(s.ctcon)
            e2:SetOperation(s.ctop)
            e2:SetReset(RESET_EVENT+RESETS_STANDARD)
            tc:RegisterEffect(e2,true)
        end
    end
end

-- Condition for Battle Phase lock
function s.bpcon(e)
    return Duel.IsBattlePhase()
end

-- Condition for absorbing a Kill Counter
function s.ctcon(e,tp,eg,ep,ev,re,r,rp)
    return e:GetHandler():IsCanAddCounter(0x199,1)
end

-- Operation to add the Kill Counter to the monster
function s.ctop(e,tp,eg,ep,ev,re,r,rp)
    local c=e:GetHandler()
    if c:IsRelateToEffect(e) and c:IsFaceup() then
        c:AddCounter(0x199,1)
    end
end