-- Slasher Jump Scare
local s,id=GetID()
function s.initial_effect(c)
    -- Activate: Special Summon from hand during Battle Phase
    local e1=Effect.CreateEffect(c)
    e1:SetCategory(CATEGORY_SPECIAL_SUMMON)
    e1:SetType(EFFECT_TYPE_ACTIVATE)
    e1:SetCode(EVENT_FREE_CHAIN)
    e1:SetHintTiming(TIMING_BATTLE_PHASE,TIMING_BATTLE_PHASE+TIMINGS_CHECK_MONSTER)
    e1:SetCondition(s.condition)
    e1:SetTarget(s.target)
    e1:SetOperation(s.activate)
    c:RegisterEffect(e1)
end

function s.condition(e,tp,eg,ep,ev,re,r,rp)
    return Duel.IsBattlePhase()
end

function s.filter(c,e,tp)
    return c:IsSetCard(0x991) and c:IsMonster() and c:IsCanBeSpecialSummoned(e,0,tp,false,false)
end

function s.target(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return Duel.GetLocationCount(tp,LOCATION_MZONE)>0
        and Duel.IsExistingMatchingCard(s.filter,tp,LOCATION_HAND,0,1,nil,e,tp) end
    Duel.SetOperationInfo(0,CATEGORY_SPECIAL_SUMMON,nil,1,tp,LOCATION_HAND)
end

function s.activate(e,tp,eg,ep,ev,re,r,rp)
    if Duel.GetLocationCount(tp,LOCATION_MZONE)<=0 then return end
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_SPSUMMON)
    local g=Duel.SelectMatchingCard(tp,s.filter,tp,LOCATION_HAND,0,1,1,nil,e,tp)
    local tc=g:GetFirst()
    if tc and Duel.SpecialSummon(tc,0,tp,tp,false,false,POS_FACEUP)>0 then
        -- Register a flag to track if this monster destroys a monster this turn
        tc:RegisterFlagEffect(id,RESET_EVENT+RESETS_STANDARD+RESET_PHASE+PHASE_END,0,1)

        -- Destroying a monster by battle clears the flag
        local e1=Effect.CreateEffect(e:GetHandler())
        e1:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_CONTINUOUS)
        e1:SetCode(EVENT_BATTLE_DESTROYING)
        e1:SetOperation(s.descheck)
        e1:SetReset(RESET_EVENT+RESETS_STANDARD+RESET_PHASE+PHASE_END)
        tc:RegisterEffect(e1,true)

        -- End Phase check to destroy it if it didn't score a kill
        local e2=Effect.CreateEffect(e:GetHandler())
        e2:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_CONTINUOUS)
        e2:SetCode(EVENT_PHASE+PHASE_END)
        e2:SetCountLimit(1)
        e2:SetLabelObject(tc)
        e2:SetOperation(s.desop)
        e2:SetReset(RESET_PHASE+PHASE_END)
        Duel.RegisterEffect(e2,tp)
    end
end

-- The summoned monster destroyed a monster by battle: it survives the End Phase
function s.descheck(e,tp,eg,ep,ev,re,r,rp)
    e:GetHandler():ResetFlagEffect(id)
end

-- End Phase destruction operation
function s.desop(e,tp,eg,ep,ev,re,r,rp)
    local tc=e:GetLabelObject()
    -- If the flag is still active, it means it never destroyed a monster this turn
    if tc and tc:GetFlagEffect(id)>0 and tc:IsLocation(LOCATION_MZONE) then
        Duel.Destroy(tc,REASON_EFFECT)
    end
end